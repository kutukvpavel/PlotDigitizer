# PlotDigitizer

Small tool designed to help with digitizing scanned plots. It expects image files (jpg, png, bmp) and outputs *.csv as well as a *.png preview of digitized dataset.

Scanned images have to be processed in advance so that color distance between traces (points) and the background is sufficient. Scanning resolution has to be high enough for a point to comrise at least 4-8 pixels (and to prevent significant color bleeding).
The background is assumed to be a (more-or-less) solid color (target background color is configurable). Alpha channel support wasn't tested.

TODO: Create a GUI for easy parameter adjustment.

# Usage

DigitizerEngine.exe "Path to preprocessed image" [-d -n:# -b:# -c:# -k -s:#]

-d = Use semicolon as CSV delimeter. Useful for certain UI cultures.

-n:# = Require at least # nearest pixels to differ from the background for a pixel to be counted as a plot point. Default is 2. Minimum value is 1, maximum value is, obviously, 8.

-b:# = Background color # as ARGB integer (https://argb-int-calculator.netlify.app/). Default is white (-1).

-c:# = Minimum euclidian color distance between specified background color and a pixel for the former to be counted as a plot point.

-k = Keep console window open after processing.

-s:# = Scaling factor for CSV output. Default is 1, you can compute this factor based on scanner resolution and original plot's properties.

# Examples

https://imgur.com/a/Ly53HMK
