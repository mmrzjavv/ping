import authService from "../auth/AuthorizeService";

export type MessageType = 1 | 2 | 3;

export interface ChatUser {
	id: number;
	fullName: string;
	username: string;
	pictureId?: string;
}

export interface Message {
	id: number;
	conversationId: number;
	content?: string;
	type: MessageType;
	attachmentFileName?: string;
	attachmentUrl?: string;
	attachmentContentType?: string;
	isSeen: boolean;
	created: string;
	seenAt?: string;
	createdBy: ChatUser;
}

export interface Conversation {
	id: number;
	title?: string;
	isPrivate: boolean;
	created: string;
	lastMessage?: Message;
	members: ChatUser[];
}

export interface UploadAttachmentResponse {
	succeeded: boolean;
	message: string;
	error?: string;
	status: number;
	data?: {
		fileName?: string;
		FileName?: string;
		url?: string;
		Url?: string;
		contentType?: string;
		ContentType?: string;
		size?: number;
		Size?: number;
	};
}

const parseErrorMessage = async (response: Response): Promise<string> => {
	const text = await response.text();
	if (!text) {
		return `Request failed with status ${response.status}`;
	}

	try {
		const payload = JSON.parse(text) as { title?: string; error?: string; message?: string };
		return payload.error || payload.message || payload.title || text;
	} catch {
		return text;
	}
};

const request = async <T>(url: string, init?: RequestInit): Promise<T> => {
	const token = await authService.getAccessToken();
	const response = await fetch(url, {
		...init,
		headers: {
			...(init?.headers || {}),
			Authorization: `Bearer ${token}`,
		},
	});

	if (!response.ok) {
		throw new Error(await parseErrorMessage(response));
	}

	if (response.status === 204) {
		return undefined as unknown as T;
	}

	return response.json();
};

export const ChatApi = {
	getConversations: () => request<Conversation[]>("/api/Chat"),
	getMessages: (conversationId: number) => request<Message[]>(`/api/Chat/${conversationId}/messages`),
	sendMessage: (body: {
		conversationId: number;
		content?: string;
		attachmentFileName?: string;
		attachmentContentType?: string;
		type: MessageType;
	}) =>
		request<{ conversationId: number; message: Message }>("/api/Chat/messages", {
			method: "POST",
			body: JSON.stringify(body),
			headers: { "Content-Type": "application/json" },
		}),
	markSeen: (messageId: number) => request<void>(`/api/Chat/messages/${messageId}/seen`, { method: "POST" }),
	uploadAttachment: async (file: File, attachmentType: "Image" | "Audio"): Promise<UploadAttachmentResponse> => {
		const token = await authService.getAccessToken();
		const form = new FormData();
		form.append("FileData", file);
		const response = await fetch(`/api/attachment/upload?attachmentType=${attachmentType}`, {
			method: "POST",
			body: form,
			headers: { Authorization: `Bearer ${token}` },
		});
		if (!response.ok) {
			throw new Error(await parseErrorMessage(response));
		}
		return response.json();
	},
};
