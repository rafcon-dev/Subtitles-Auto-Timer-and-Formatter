# Subtitles Auto Timer and Formatter

An old program to automatically take a video script, and create timed and formatted subtitles from it.

It takes as input the video, and based on the difference between frames and audio, tries to find the best subtitles formatting and timing.

Was useful when youtube still didn't do this kind of stuff automatically, and machine learning wasn't really a thing. It's pretty much abandonware at this point.

It dependes on Microsoft.Speech, which is included in Microsoft Speech SDK. However, Microsoft no longer supports this SDK, and even removed it from their website apparently. So, this program is now impossible to compile without refactoring those parts with something like the new Cognitive Services Speech SDK.
