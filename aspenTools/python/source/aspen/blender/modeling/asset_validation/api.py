import re
# Regex list of default mesh name patterns Blender uses. Maybe add more?
default_patterns = [
    r"^Cube(\.\d+)?$",
    r"^Circle(\.\d+)?$",
    r"^Plane(\.\d+)?$",
    r"^Sphere(\.\d+)?$",
    r"^Icosphere(\.\d+)?$",
    r"^Cone(\.\d+)?$",
    r"^Cylinder(\.\d+)?$",
    r"^Torus(\.\d+)?$",
    r"^Suzanne(\.\d+)?$",
]

def is_default_name(name: str):
    """Check if a string matches a Blender default object pattern.
    Args:
        name (str): String to check.
    """
    return any(re.match(p, name) for p in default_patterns)
