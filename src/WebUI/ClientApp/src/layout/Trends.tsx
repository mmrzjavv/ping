import React, { useEffect, useMemo, useState } from "react";
import TimeAgo from "javascript-time-ago";
import en from "javascript-time-ago/locale/en";

interface TrendDto {
	tag: string;
	postCount: number;
	likeCount: number;
	recordedAt: string;
}

TimeAgo.addLocale(en);
const timeAgo = new TimeAgo("en-US");

const Trends: React.FC = () => {
	const [trends, setTrends] = useState<TrendDto[]>([]);
	const [isLoading, setIsLoading] = useState(true);
	const [errorMessage, setErrorMessage] = useState("");

	useEffect(() => {
		const loadTrends = async () => {
			try {
				const response = await fetch("/api/Trends?Count=5");
				if (!response.ok) {
					throw new Error("Unable to load trends");
				}
				const data: TrendDto[] = await response.json();
				setTrends(data);
			} catch (error) {
				setErrorMessage((error as Error).message);
			} finally {
				setIsLoading(false);
			}
		};

		loadTrends();
	}, []);

	const headerText = useMemo(() => {
		if (isLoading) return "Loading trends";
		if (errorMessage) return "Trends unavailable";
		return "Trends for you";
	}, [isLoading, errorMessage]);

	return (
		<React.Fragment>
			<h3 className="font-bold border-b py-3 px-4 text-xl">{headerText}</h3>
			{isLoading && <div className="px-4 py-4 text-sm text-gray-500">Fetching the latest trends...</div>}
			{errorMessage && (
				<div className="px-4 py-4 text-sm text-red-500">{errorMessage}</div>
			)}
			{!isLoading && !trends.length && !errorMessage && (
				<div className="px-4 py-3 text-sm text-gray-500">No trends available right now.</div>
			)}
			{trends.map((trend) => (
				<div key={trend.tag} className="border-b py-3 px-4 text-sm">
					<div className="text-gray-600 tracking-wide text-xs uppercase">Trending</div>
					<h2 className="font-semibold text-lg">{trend.tag}</h2>
					<div className="text-gray-600 text-xs mb-1">
						{timeAgo.format(new Date(trend.recordedAt))}
					</div>
					<div className="flex flex-wrap text-xs text-gray-600 gap-3">
						<span>{trend.postCount.toLocaleString()} Posts</span>
						<span>{trend.likeCount.toLocaleString()} Likes</span>
					</div>
				</div>
			))}
			<div className="text-primary py-3 px-4">Show more</div>
		</React.Fragment>
	);
};

export default Trends;
