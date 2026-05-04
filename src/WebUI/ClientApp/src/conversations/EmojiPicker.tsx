import React from "react";

const emojis = ["😀", "😂", "😍", "🔥", "👏", "🎉", "❤️", "👍", "😎", "🙏"];

interface EmojiPickerProps {
	onSelect: (emoji: string) => void;
}

const EmojiPicker: React.FC<EmojiPickerProps> = ({ onSelect }) => (
	<div className="absolute bottom-14 left-0 bg-white border rounded-xl shadow-lg p-2 grid grid-cols-5 gap-2 z-10">
		{emojis.map((emoji) => (
			<button key={emoji} className="text-xl hover:bg-gray-100 rounded p-1" onClick={() => onSelect(emoji)}>
				{emoji}
			</button>
		))}
	</div>
);

export default EmojiPicker;
