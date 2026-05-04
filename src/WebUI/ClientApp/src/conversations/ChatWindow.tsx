import React, { useEffect, useMemo, useRef, useState } from "react";
import { useParams } from "react-router-dom";
import authService from "../auth/AuthorizeService";
import Button from "../shared/Button";
import FileInput from "../shared/FileInput";
import { ChatApi, Message } from "./chatApi";
import { useChatHub } from "./useChatHub";
import EmojiPicker from "./EmojiPicker";
import AudioRecorder from "./AudioRecorder";

interface ChatWindowProps {
	onMessageReceived: (message: Message) => void;
	onConversationRefresh: VoidFunction;
}

const ChatWindow: React.FC<ChatWindowProps> = ({ onMessageReceived, onConversationRefresh }) => {
	const { conversationId } = useParams<{ conversationId: string }>();
	const id = Number(conversationId);
	const [messages, setMessages] = useState<Message[]>([]);
	const [text, setText] = useState("");
	const [showEmoji, setShowEmoji] = useState(false);
	const [domainUserId, setDomainUserId] = useState<number>();
	const scrollRef = useRef<HTMLDivElement>(null);
	const hub = useChatHub(id);

	const loadMessages = async () => {
		const items = await ChatApi.getMessages(id);
		setMessages(items);
	};

	useEffect(() => {
		loadMessages();
		authService.getDomainUser().then((user) => setDomainUserId(user?.id));
	}, [id]);

	useEffect(() => {
		if (!hub) return;

		hub.on("ReceiveMessage", (message: Message) => {
			setMessages((current) => [...current, message]);
			onMessageReceived(message);
			onConversationRefresh();
		});

		hub.on("Seen", ({ messageId }: { messageId: number }) => {
			setMessages((current) => current.map((message) => (message.id === messageId ? { ...message, isSeen: true } : message)));
		});

		return () => {
			hub.off("ReceiveMessage");
			hub.off("Seen");
		};
	}, [hub, onConversationRefresh, onMessageReceived]);

	useEffect(() => {
		scrollRef.current?.scrollIntoView({ behavior: "smooth" });
		const unseen = messages.filter((message) => !message.isSeen && message.createdBy.id !== domainUserId);
		unseen.forEach((message) => ChatApi.markSeen(message.id));
	}, [messages, domainUserId]);

	const send = async () => {
		if (!text.trim()) return;
		await ChatApi.sendMessage({ conversationId: id, content: text.trim(), type: 1 });
		setText("");
	};

	const uploadAndSend = async (file: File, type: "Image" | "Audio") => {
		const uploaded = await ChatApi.uploadAttachment(file, type);
		const fileName = uploaded.data?.fileName || uploaded.data?.FileName;
		if (!fileName) return;
		await ChatApi.sendMessage({
			conversationId: id,
			type: type === "Audio" ? 3 : 2,
			attachmentFileName: fileName,
			attachmentContentType: file.type,
		});
	};

	const grouped = useMemo(() => messages, [messages]);

	return (
		<div className="hidden md:flex flex-col flex-grow">
			<div className="border-b px-4 py-3 font-semibold">Conversation</div>
			<div className="flex-grow overflow-y-auto p-4 bg-gray-50">
				{grouped.map((message) => {
					const mine = message.createdBy.id === domainUserId;
					return (
						<div key={message.id} className={`flex mb-3 ${mine ? "justify-end" : "justify-start"}`}>
							<div className={`${mine ? "bg-primary text-white" : "bg-white"} rounded-2xl px-4 py-2 max-w-md shadow-sm`}>
								{message.type === 2 && !!message.attachmentUrl && (
									<img src={message.attachmentUrl} className="rounded-xl mb-2 max-h-64 w-full" alt="attachment" />
								)}
								{message.type === 3 && !!message.attachmentUrl && (
									<audio controls className="w-64">
										<source src={message.attachmentUrl} type={message.attachmentContentType || "audio/webm"} />
									</audio>
								)}
								{!!message.content && <div>{message.content}</div>}
							</div>
						</div>
					);
				})}
				<div ref={scrollRef} />
			</div>
			<div className="border-t p-3 bg-white">
				<div className="relative flex items-center gap-2">
					<Button type="button" light className="px-3 py-2" onClick={() => setShowEmoji((value) => !value)}>
						😊
					</Button>
					{showEmoji && <EmojiPicker onSelect={(emoji) => setText((value) => `${value}${emoji}`)} />}
					<FileInput accept=".png,.jpg,.jpeg,image/jpeg,image/png,image/webp" onFileChange={(file) => uploadAndSend(file, "Image")}>
						<Button type="button" light className="px-3 py-2">
							Image
						</Button>
					</FileInput>
					<AudioRecorder onRecorded={(file) => uploadAndSend(file, "Audio")} />
					<input
						className="flex-grow border rounded-full px-4 py-2 focus:outline-none"
						value={text}
						onChange={(event) => setText(event.target.value)}
						onKeyDown={(event) => {
							if (event.key === "Enter") {
								send();
							}
						}}
						onFocus={() => hub?.invoke("Typing", id).catch(() => undefined)}
						placeholder="Start a message"
					/>
					<Button type="button" className="px-4 py-2" onClick={send}>
						Send
					</Button>
				</div>
			</div>
		</div>
	);
};

export default ChatWindow;
