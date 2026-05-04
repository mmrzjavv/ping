import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useState } from "react";
import authService from "../auth/AuthorizeService";

export const useChatHub = (conversationId?: number) => {
	const [connection, setConnection] = useState<HubConnection>();

	useEffect(() => {
		let disposed = false;
		let activeConnection: HubConnection;

		const start = async () => {
			const hub = new HubConnectionBuilder()
				.withAutomaticReconnect()
				.withUrl("/hubs/chat", { accessTokenFactory: async () => (await authService.getAccessToken()) || "" })
				.build();

			try {
				await hub.start();
				if (conversationId) {
					await hub.invoke("JoinConversation", conversationId);
				}
				if (!disposed) {
					activeConnection = hub;
					setConnection(hub);
				}
			} catch (error) {
				console.error("Failed to start chat hub connection.", error);
				if (!disposed) {
					setConnection(undefined);
				}
				hub.stop().catch(() => undefined);
			}
		};

		start().catch(() => undefined);

		return () => {
			disposed = true;
			if (activeConnection) {
				if (conversationId) {
					activeConnection.invoke("LeaveConversation", conversationId).catch(() => undefined);
				}
				activeConnection.stop().catch(() => undefined);
			}
		};
	}, [conversationId]);

	return connection;
};
