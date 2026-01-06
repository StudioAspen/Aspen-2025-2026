import os

import aspen.core.os.path as aspen_path

REPO_DIR = aspen_path.get_parent_directory(os.path.abspath(__file__), 5)
PYTHON_DIR = os.path.join(REPO_DIR, 'aspenTools', 'python')
PYTHON_IMAGES_DIR = os.path.join(PYTHON_DIR, 'images')
UNITY_PROJECT_ASSETS_DIR = os.path.join(REPO_DIR, 'charonsCorner', 'Assets')

BLENDER_PROJECT_ASSETS_DIR = os.path.join(REPO_DIR, 'art')
"""Refers to the Artists' main directory (Aspen-2025-2026 > charonsCorner > art).
To refer to subfolders specifically, write the following: 

BLENDER_PROJECT_ASSETS_DIR, '[insert subfolder1's name here]', '[insert subfolder1's name here]', ...'
You can also substitute subfolder names for variables you created."""