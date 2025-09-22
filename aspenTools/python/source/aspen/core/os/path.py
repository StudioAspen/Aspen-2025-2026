# Property of Guardian's Lament
# Author: Mikyle Mosquera
# 2025Q3

import os

def get_parent_directory(file_path: str, count: int = 1) -> str:
    """Go up parent directory a specified number of times.

    Args:
        file_path (string): Path to start at.
        count (int, optional): Defaults to 1.
    """
    current_path = file_path
    for i in range(count):
        current_path = os.path.dirname(current_path)

    return current_path


def search_parents_for_directory_recursive(current_directory: str, directory_name: str) -> str:
    """Search parent directories recursively for a specific directory.

    Args:
        current_directory (str): The current directory.
        directory_name (str): The directory name.
    """
    # Check if found directory. If found, return it.
    if os.path.basename(current_directory) == directory_name:
        return current_directory

    # Get the parent directory, and check if it's the same as current. If so, hit root and return empty string.
    parent_directory = os.path.dirname(current_directory)
    if parent_directory == current_directory:
        return ''

    return search_parents_for_directory_recursive(parent_directory, directory_name)