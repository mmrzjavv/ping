import React from "react";
import FollowSuggestions from "./FollowSuggestions";
import Trends from "./Trends";
import UserSearch from "./UserSearch";

const Sidebar: React.FC = () => {
	return (
		<div className="hidden lg:block w-350px flex-shrink-0">
			<div className="p-3 pl-7 border-l h-full w-350px fixed">
				<UserSearch />
				<div className="rounded-xl bg-gray-100 mt-4">
					<Trends />
				</div>
				<div className="rounded-xl bg-gray-100 mt-4">
					<FollowSuggestions />
				</div>
			
			</div>
		</div>
	);
};

export default Sidebar;
