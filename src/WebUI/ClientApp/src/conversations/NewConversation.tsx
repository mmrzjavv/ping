import { faSearch, faTimes, faUser } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React, { useEffect, useRef, useState } from "react";
import {
    CreateConversationCommand,
    ISearchUserDto,
    UsersClient,
    ConversationsClient,
} from "../core/WebApiClient";
import Header from "../shared/Header";
import LoadingIndicator from "../shared/LoadingIndicator";
import Modal from "../shared/Modal";
import UserPicture from "../user/UserPicture";
import { useHistory } from "react-router-dom";

interface NewConversationProps {
    onClose: VoidFunction;
}

const NewConversation: React.FC<NewConversationProps> = ({ onClose }) => {
    const history = useHistory();

    const [state, setState] = useState({
        search: "",
        users: [] as ISearchUserDto[],
        loading: false,
        selectedUsers: [] as ISearchUserDto[],
    });

    const timeoutRef = useRef<NodeJS.Timeout>();

    // ------------------ جست‌وجوی کاربران ------------------
    const queryUsers = async () => {
        const searchValue = state.search.trim();

        if (!searchValue) {
            setState(prev => ({ ...prev, users: [], loading: false }));
            return;
        }

        setState(prev => ({ ...prev, loading: true, users: [] }));

        try {
            const users = await new UsersClient().searchUser(searchValue);
            setState(prev => ({ ...prev, loading: false, users }));
        } catch (error) {
            console.error("Error searching users:", error);
            setState(prev => ({ ...prev, loading: false }));
        }
    };

    // debounce برای کاهش تعداد درخواست‌ها
    useEffect(() => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current);
        timeoutRef.current = setTimeout(queryUsers, 500);

        return () => {
            if (timeoutRef.current) clearTimeout(timeoutRef.current);
        };
    }, [state.search]);

    // ------------------ ساخت کانورسیشن ------------------
    const createConversation = async () => {
        if (!state.selectedUsers.length) return;
        try {
            const ids = state.selectedUsers.map(u => u.id);
            const command = new CreateConversationCommand({ members: ids });
            const conversationId = await new ConversationsClient().create(command);

            // هدایت به صفحه چت
            history.push(`/chat/${conversationId}`);
            onClose();
        } catch (error) {
            console.error("Error creating conversation:", error);
        }
    };

    // ------------------ رندر کامپوننت ------------------
    return (
        <Modal className="h-600px">
            <Header
                title="New message"
                leftButton={{ icon: faTimes, onClick: onClose }}
                rightButton={{
                    text: "Next",
                    onClick: createConversation,
                    disabled: state.selectedUsers.length === 0,
                }}
            >
                {/* فیلد سرچ */}
                <div className="flex items-center mb-2">
                    <FontAwesomeIcon className="mx-5 text-primary" icon={faSearch} />
                    <input
                        type="text"
                        value={state.search}
                        placeholder="Search people"
                        className="focus:outline-none flex-grow text-gray-500"
                        onChange={(e) => {
                            const value = e.target.value; // جدا کردن مقدار از رویداد برای جلوگیری از event pooling
                            setState(prev => ({
                                ...prev,
                                search: value.trimStart(),
                            }));
                        }}
                    />
                </div>

                {/* کاربران انتخاب‌شده */}
                {state.selectedUsers.length > 0 && (
                    <div className="flex mx-2 flex-wrap">
                        {state.selectedUsers.map(u => (
                            <div
                                key={u.id}
                                className="rounded-full flex p-1 border items-center mb-2 mr-2 cursor-pointer hover:bg-primary hover:bg-opacity-10"
                                onClick={() =>
                                    setState(prev => ({
                                        ...prev,
                                        selectedUsers: prev.selectedUsers.filter(s => s.id !== u.id),
                                    }))
                                }
                            >
                                <UserPicture pictureId={u.pictureId} className="h-6 w-6 mr-3" />
                                <div className="font-bold">{u.fullName}</div>
                                <FontAwesomeIcon icon={faTimes} className="text-primary mx-2" />
                            </div>
                        ))}
                    </div>
                )}
            </Header>

            {/* نتایج جست‌وجو */}
            <div className="flex overflow-y-auto">
                {state.loading && <LoadingIndicator className="mx-auto" />}
                {!state.loading && state.users.length > 0 && (
                    <ul className="w-full">
                        {state.users.map(u => (
                            <li
                                key={u.id}
                                className="p-3 border-b hover:bg-gray-50 cursor-pointer"
                                onClick={() =>
                                    setState(prev => ({
                                        ...prev,
                                        selectedUsers: [...prev.selectedUsers, u],
                                        search: "",
                                        users: [],
                                    }))
                                }
                            >
                                {!!u.followedByMe && (
                                    <div className="font-bold text-gray-500 text-sm ml-5 mb-1">
                                        <FontAwesomeIcon icon={faUser} className="mr-3" />
                                        Following
                                    </div>
                                )}
                                <div className="flex">
                                    <UserPicture className="h-9 w-9" pictureId={u.pictureId} />
                                    <div className="flex flex-col ml-2 flex-grow justify-between">
                                        <h2 className="font-semibold leading-none">{u.fullName}</h2>
                                        <span className="text-gray-500 font-light leading-none">
                                            @{u.username}
                                        </span>
                                    </div>
                                </div>
                            </li>
                        ))}
                    </ul>
                )}
            </div>
        </Modal>
    );
};

export default NewConversation;
