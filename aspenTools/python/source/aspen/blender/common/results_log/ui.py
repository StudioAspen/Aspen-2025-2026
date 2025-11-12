import logging
from .api import ResultLogMainWindow, ResultLogHandler

# Declared it here so that we could have a persistent window in a Blender session.
# Possible error with script reloading... see suggestion from coderabbit on PR
g_ResultLogMainWindow = ResultLogMainWindow()

# Creating a handler & formatter for the aspen logger. I declared it here b/c I wanted to pass
# in the window defined in this file, but I'm not sure if there's a better practice.
aspenLogger = logging.getLogger("aspen")

# TODO: I'd like to create a handler/formatter for DEBUG that would also print filename and other helpful information.
handler = ResultLogHandler(g_ResultLogMainWindow)
handler.setLevel(logging.INFO)
handler.set_name("ResultLogHandler")

formatter = logging.Formatter('%(asctime)s - %(message)s')
handler.setFormatter(formatter)

aspenLogger.addHandler(handler)

def show_result_log_window():
    """ This makes the window show itself. """
    g_ResultLogMainWindow.show()

def test_result_log_window():
    """ Test function to see if the flags are working. """
    g_ResultLogMainWindow.test_print_log_levels()
