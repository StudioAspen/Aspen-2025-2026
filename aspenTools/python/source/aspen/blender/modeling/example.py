import logging # This is the Python module.
from aspen.blender.common.results_log import ui

logger = logging.getLogger("aspen") # IMPORTANT! The module returns loggers by name. The wrong name will return a DIFFERENT logger.

# Example usages
logger.info("The tool ran successfully!")
logger.warning("Unfilled face detected in mesh, operation ran successfully but results may be incorrect.")
logger.error("Could not complete operation. File was not overwritten and no output was created.")

# Causes the window to display itself.
ui.show_result_log_window()