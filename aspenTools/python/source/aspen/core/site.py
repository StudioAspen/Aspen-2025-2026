import os

import aspen.core.os_utils as os_utils

__all__ = ['PROJECT_ROOT', 'UNITY_ASSETS_ROOT']

PROJECT_ROOT = os_utils.get_parent_directory(os.path.abspath(__file__), 6)
UNITY_ASSETS_ROOT = os.path.join(PROJECT_ROOT, 'charonsCorner', 'Assets')