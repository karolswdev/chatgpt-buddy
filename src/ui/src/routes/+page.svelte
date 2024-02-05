<script lang="ts">
	import { onMount } from "svelte";

	let canvas: HTMLCanvasElement;
	let record: HTMLButtonElement;
	let stop: HTMLButtonElement;
	let soundClips: HTMLElement;
	let audioCtx: AudioContext;

	onMount(async () => {
		stop.disabled = true;
		const canvasCtx = canvas.getContext("2d") as CanvasRenderingContext2D;
		if (navigator.mediaDevices.getUserMedia) {
			console.log("The mediaDevices.getUserMedia() method is supported.");

			const constraints = { audio: true };
			let chunks: BlobPart[] = [];

			let onSuccess = function (stream: MediaStream) {
				const mediaRecorder = new MediaRecorder(stream);

				visualize(stream);

				record.onclick = function () {
					mediaRecorder.start();
					console.log(mediaRecorder.state);
					console.log("Recorder started.");
					record.style.background = "red";

					stop.disabled = false;
					record.disabled = true;
				};

				stop.onclick = function () {
					mediaRecorder.stop();
					console.log(mediaRecorder.state);
					console.log("Recorder stopped.");
					record.style.background = "";
					record.style.color = "";

					stop.disabled = true;
					record.disabled = false;
				};

				mediaRecorder.onstop = function (e) {
					console.log("Last data to read (after MediaRecorder.stop() called).");

					const clipName = prompt("Enter a name for your sound clip?", "My unnamed clip");

					const clipContainer = document.createElement("article");
					const clipLabel = document.createElement("p");
					const audio = document.createElement("audio");
					const deleteButton = document.createElement("button");
					const sendButton = document.createElement("button");

					clipContainer.classList.add("clip");
					audio.setAttribute("controls", "");
					deleteButton.textContent = "Delete";
					deleteButton.className = "delete";
					sendButton.textContent = "Send";
					sendButton.className = "block";

					if (clipName === null) {
						clipLabel.textContent = "My unnamed clip";
					} else {
						clipLabel.textContent = clipName;
					}

					clipContainer.appendChild(audio);
					clipContainer.appendChild(clipLabel);
					clipContainer.appendChild(deleteButton);
					clipContainer.appendChild(sendButton);
					soundClips.appendChild(clipContainer);

					audio.controls = true;
					const blob = new Blob(chunks, { type: mediaRecorder.mimeType });
					chunks = [];
					const audioURL = window.URL.createObjectURL(blob);
					audio.src = audioURL;
					console.log("recorder stopped");

					deleteButton.onclick = function (e: MouseEvent) {
						(e.target as HTMLElement).closest(".clip")?.remove();
					};

					// sendButton.onclick = function () {
					// 	const formData = new FormData();
					// 	formData.append("audio", blob, "recorded_audio.wav");
					// 	fetch("/upload-audio", {
					// 		method: "POST",
					// 		body: formData
					// 	}).then((response) => {
					// 		if (response.ok) {
					// 			alert("SUCCESS: Audio file uploaded");
					// 		} else {
					// 			alert("ERROR: Audio file failed to upload");
					// 		}
					// 	});
					// };

                    console.log('mime type');
					console.log(mediaRecorder.mimeType);

					sendButton.onclick = function () {
						fetch("http://localhost:3000/upload-audio", {
							method: "POST",
							body: blob,
							headers: {
								"Content-Type": 'audio/mp4'
							}
						}).then((response) => {
							if (response.ok) {
								alert("SUCCESS: Audio file uploaded");
							} else {
								alert("FAILED: Audio file failed to upload");
							}
						});
					};

					clipLabel.onclick = function () {
						const existingName = clipLabel.textContent;
						const newClipName = prompt("Enter a new name for your sound clip?");
						if (newClipName === null) {
							clipLabel.textContent = existingName;
						} else {
							clipLabel.textContent = newClipName;
						}
					};
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

				canvasCtx.fillStyle = "rgb(200, 200, 200)";
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

<div class="container m-auto py-4">
	<header>
		<h1 class="text-2xl font-bold">🤖 Chat GPT Buddy</h1>
	</header>

	<section class="main-controls">
		<canvas bind:this={canvas} height="60px"></canvas>
		<div id="buttons">
			<button bind:this={record} class="rounded-md bg-slate-950 p-2 font-bold text-slate-200"
				>Record</button
			>
			<button bind:this={stop} class="rounded-md bg-slate-950 p-2 font-bold text-slate-200"
				>Stop</button
			>
		</div>
	</section>
	<section bind:this={soundClips}></section>
</div>
