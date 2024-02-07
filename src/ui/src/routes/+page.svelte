<script lang="ts">
	import { onMount } from "svelte";

	let canvas: HTMLCanvasElement;
	let audioCtx: AudioContext;
	let mediaRecorder: MediaRecorder;
	let audioUrls: string[] = [];
	let isRecording = false;

	const toggleRecord = () => {
		if (!isRecording) {
			mediaRecorder.start();
			console.log(mediaRecorder.state);
			console.log("Recorder started.");

			isRecording = true;
		} else {
			mediaRecorder.stop();
			console.log(mediaRecorder.state);
			console.log("Recorder stopped.");

			isRecording = false;
		}
	};

	onMount(async () => {
		const canvasCtx = canvas.getContext("2d") as CanvasRenderingContext2D;
		if (navigator.mediaDevices.getUserMedia) {
			console.log("The mediaDevices.getUserMedia() method is supported.");

			const constraints = { audio: true };
			let chunks: BlobPart[] = [];

			let onSuccess = function (stream: MediaStream) {
				mediaRecorder = new MediaRecorder(stream, { mimeType: "audio/mp4" });

				visualize(stream);

				mediaRecorder.onstop = async function (e) {
					console.log("Last data to read (after MediaRecorder.stop() called).");

					const blob = new Blob(chunks, { type: mediaRecorder.mimeType });
					chunks = [];
					const audioURL = window.URL.createObjectURL(blob);
					audioUrls = [...audioUrls, audioURL];

					try {
						const res = await fetch("http://localhost:3000/upload-audio", {
							method: "POST",
							body: blob,
							headers: {
								"Content-Type": mediaRecorder.mimeType
							}
						});

						if (res.ok) {
							alert("Success uploading audio");
						} else {
							alert("Failed uploading audio");
						}
					} catch (e) {
						alert("Failed uploading audio");
					}
				};

				mediaRecorder.ondataavailable = function (e) {
					chunks.push(e.data);
				};
			};

			let onError = function (err: string) {
				console.log("The following error occured: " + err);
			};

			navigator.mediaDevices.getUserMedia(constraints).then(onSuccess, onError);
		} else {
			console.log("MediaDevices.getUserMedia() not supported on your browser!");
		}

		function visualize(stream: MediaStream) {
			if (!audioCtx) {
				audioCtx = new AudioContext();
			}

			const source = audioCtx.createMediaStreamSource(stream);

			const analyser = audioCtx.createAnalyser();
			analyser.fftSize = 2048;
			const bufferLength = analyser.frequencyBinCount;
			const dataArray = new Uint8Array(bufferLength);

			source.connect(analyser);

			draw();

			function draw() {
				const WIDTH = canvas.width;
				const HEIGHT = canvas.height;

				requestAnimationFrame(draw);

				analyser.getByteTimeDomainData(dataArray);

				canvasCtx.fillStyle = "rgb(150, 255, 200)";
				canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);

				canvasCtx.lineWidth = 2;
				canvasCtx.strokeStyle = "rgb(0, 0, 0)";

				canvasCtx.beginPath();

				let sliceWidth = (WIDTH * 1.0) / bufferLength;
				let x = 0;

				for (let i = 0; i < bufferLength; i++) {
					let v = dataArray[i] / 128.0;
					let y = (v * HEIGHT) / 2;

					if (i === 0) {
						canvasCtx.moveTo(x, y);
					} else {
						canvasCtx.lineTo(x, y);
					}

					x += sliceWidth;
				}

				canvasCtx.lineTo(canvas.width, canvas.height / 2);
				canvasCtx.stroke();
			}
		}
	});
</script>

<div class="relative m-auto flex flex-col items-center py-12">
	<header>
		<h1
			class="text-secondary-foreground bg-secondary relative z-50 -mb-8 rounded-lg p-4 text-4xl font-bold"
		>
			🤖 Chat GPT Buddy
		</h1>
	</header>

	<section class="relative py-2">
		<canvas bind:this={canvas} height="100px" class="my-2 rounded-xl"></canvas>
		<div class="flex justify-center py-2">
			<button
				on:click={toggleRecord}
				class={`bg-secondary text-secondary-foreground rounded-md p-2 font-bold ${isRecording && "bg-red-600"} z-50 -mt-8`}
				>{isRecording ? "stop" : "record"}</button
			>
		</div>
	</section>
	<secion class="py-2">
		<p>clips</p>
		{#each audioUrls as url}
			<p>generating summary ...</p>
			<audio src={url} controls />
		{/each}
	</secion>
</div>
