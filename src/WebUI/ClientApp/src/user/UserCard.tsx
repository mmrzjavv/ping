import React from "react";
import { faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { NavLink } from "react-router-dom";
import { ApplicationPaths } from "../auth/ApiAuthorizationConstants";

export const UserCard: React.FC = () => {
	return (
		<NavLink
			className="flex mt-auto items-center justify-center hover-btn p-2 w-full md:w-auto md:justify-start"
			to={{ pathname: `${ApplicationPaths.LogOut}`, state: { local: true } }}
		>
			<div className="flex h-10 w-10 items-center justify-center rounded-full bg-gray-200 text-xl text-gray-600">
				<FontAwesomeIcon icon={faSignOutAlt} />
			</div>
			<span className="ml-4 text-sm font-semibold hidden md:inline">Logout</span>
		</NavLink>
	);
};
