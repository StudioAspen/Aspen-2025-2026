import bpy


PUBLISH_TYPE_ENUM_CHARACTER = 'CHARACTER'
"""Export Type: Character"""

PUBLISH_TYPE_ENUM_ACTOR = 'ACTOR'
"""Export Type: Actor"""


PUBLISH_TYPE_ENUM_ITEMS = [
    (PUBLISH_TYPE_ENUM_CHARACTER, PUBLISH_TYPE_ENUM_CHARACTER.title(), ''),
    (PUBLISH_TYPE_ENUM_ACTOR, PUBLISH_TYPE_ENUM_ACTOR.title(), '')
]

ASSET_TYPE_ENUM_ITEMS = [
    ('CHARACTER', 'Character', ''), #item #0. 'CHARACTER' is the "identifier", 'Character' is how it appears in the UI.
    ('ACTOR', 'Actor', ''), #item #1 and so forth...
]

