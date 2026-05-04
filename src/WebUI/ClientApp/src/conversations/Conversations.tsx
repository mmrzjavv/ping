import React, { useEffect, useMemo, useState } from "react";
import { Route, Switch, useHistory, useRouteMatch } from "react-router-dom";
import Header from "../shared/Header";
import NewConversation from "./NewConversation";
import Placeholder from "./Placeholder";
import { ChatApi, Conversation, Message } from "./chatApi";
import ConversationsList from "./ConversationsList";
import ChatWindow from "./ChatWindow";

const Conversations: React.FC = () => {
	const [showModal, setShowModal] = useState(false);
	const [conversations, setConversations] = useState<Conversation[]>([]);
	const [loadError, setLoadError] = useState<string>();
	const history = useHistory();
	const match = useRouteMatch();

	const loadConversations = async () => {
		try {
			const items = await ChatApi.getConversations();
			setConversations(items);
			setLoadError(undefined);
		} catch (error) {
			setConversations([]);
			setLoadError(error instanceof Error ? error.message : "Failed to load conversations.");
		}
	};

	useEffect(() => {
		loadConversations();
	}, []);

	const onMessageReceived = (message: Message) => {
		setConversations((current) => {
			const updated = [...current];
			const index = updated.findIndex((conversation) => conversation.id === message.conversationId);
			if (index >= 0) {
				updated[index] = { ...updated[index], lastMessage: message };
				const [conversation] = updated.splice(index, 1);
				updated.unshift(conversation);
			}
			return updated;
		});
	};

	const emptyState = useMemo(
		() => (
			<div className="flex-grow flex">
				<Placeholder
					className="mx-auto self-center"
					title="You don’t have a message selected"
					description="Choose one from your existing messages, or start a new one"
					button={{ onClick: () => setShowModal(true), text: "New message" }}
				/>
			</div>
		),
		[]
	);

	return (
		<div className="flex flex-grow min-h-0">
			{showModal && <NewConversation onClose={() => setShowModal(false)} />}
			<div className="border-r w-full md:w-2/5 flex flex-col">
				<Header title="Chat" rightButton={{ text: "New", onClick: () => setShowModal(true) }} />
				{loadError && <div className="px-4 py-3 text-sm text-red-600 border-b bg-red-50">{loadError}</div>}
				<ConversationsList
					conversations={conversations}
					onSelect={(conversationId) => history.push(`${match.url}/${conversationId}`)}
				/>
			</div>
			<Switch>
				<Route path={`${match.path}/:conversationId`}>
					<ChatWindow onMessageReceived={onMessageReceived} onConversationRefresh={loadConversations} />
				</Route>
				<Route path={match.path}>{emptyState}</Route>
			</Switch>
		</div>
	);
};

export default Conversations;
