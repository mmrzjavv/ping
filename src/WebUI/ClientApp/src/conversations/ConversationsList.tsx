import React from "react";
import { Conversation } from "./chatApi";

interface ConversationsListProps {
	conversations: Conversation[];
	onSelect: (conversationId: number) => void;
}

const ConversationsList: React.FC<ConversationsListProps> = ({ conversations, onSelect }) => {
	return (
		<div className="overflow-y-auto flex-grow">
			{conversations.map((conversation) => {
				const otherMember = conversation.members[0];
				return (
					<button
						key={conversation.id}
						className="w-full text-left px-4 py-3 border-b hover:bg-gray-50"
						onClick={() => onSelect(conversation.id)}
					>
						<div className="font-semibold">{conversation.title || otherMember?.fullName || "Conversation"}</div>
						<div className="text-sm text-gray-500 truncate">
							{conversation.lastMessage?.type === 3
								? "Voice message"
								: conversation.lastMessage?.type === 2
								? "Image"
								: conversation.lastMessage?.content || "No messages yet"}
						</div>
					</button>
				);
			})}
		</div>
	);
};

export default ConversationsList;
