import React, { useRef, useState } from "react";
import Button from "../shared/Button";

interface AudioRecorderProps {
	onRecorded: (file: File) => void;
}

type BrowserMediaRecorder = {
	mimeType: string;
	ondataavailable: ((event: { data: Blob }) => void) | null;
	onstop: (() => void) | null;
	start: () => void;
	stop: () => void;
};

type MediaRecorderConstructor = new (stream: MediaStream) => BrowserMediaRecorder;

const AudioRecorder: React.FC<AudioRecorderProps> = ({ onRecorded }) => {
	const [recording, setRecording] = useState(false);
	const recorderRef = useRef<BrowserMediaRecorder | null>(null);
	const chunksRef = useRef<Blob[]>([]);

	const toggleRecording = async () => {
		if (recording) {
			recorderRef.current?.stop();
			setRecording(false);
			return;
		}

		const mediaRecorderFactory = (window as Window & {
			MediaRecorder?: MediaRecorderConstructor;
		}).MediaRecorder;

		if (!navigator.mediaDevices?.getUserMedia || !mediaRecorderFactory) {
			return;
		}

		const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
		const recorder = new mediaRecorderFactory(stream);
		chunksRef.current = [];
		recorder.ondataavailable = (event) => chunksRef.current.push(event.data);
		recorder.onstop = () => {
			const blob = new Blob(chunksRef.current, { type: recorder.mimeType || "audio/webm" });
			const file = new File([blob], `voice-${Date.now()}.webm`, { type: blob.type });
			onRecorded(file);
			stream.getTracks().forEach((track) => track.stop());
		};
		recorder.start();
		recorderRef.current = recorder;
		setRecording(true);
	};

	return (
		<Button type="button" light className="px-3 py-2" onClick={toggleRecording}>
			{recording ? "Stop" : "Voice"}
		</Button>
	);
};

export default AudioRecorder;
