import os

def get_parent_directory(path: str, num: int):
    """Get the parent directory of the specified file path.

    Args:
        path (str): The file path.
        num (int): The number of times to get the parent directory.

    Returns:
        str: The parent directory.
    """
    result = path
    count = 0
    while count < num:
        result = os.path.dirname(result)

    return result