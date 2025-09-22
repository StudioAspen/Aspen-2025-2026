import os

import aspen.core.os.path as aspen_path

REPO_DIR = aspen_path.get_parent_directory(os.path.abspath(__file__), 5)
PYTHON_DIR = os.path.join(REPO_DIR, 'aspenTools', 'python')
PYTHON_IMAGES_DIR = os.path.join(PYTHON_DIR, 'images')
UNITY_PROJECT_ASSETS_DIR = os.path.join(REPO_DIR, 'charonsCorner', 'Assets')