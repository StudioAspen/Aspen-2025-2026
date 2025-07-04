"""


Cachefile Operators
*******************

:func:`layer_add`

:func:`layer_move`

:func:`layer_remove`

:func:`open`

:func:`reload`

"""

import typing

def layer_add(filepath: str = '', hide_props_region: bool = True, filter_blender: bool = False, filter_backup: bool = False, filter_image: bool = False, filter_movie: bool = False, filter_python: bool = False, filter_font: bool = False, filter_sound: bool = False, filter_text: bool = False, filter_archive: bool = False, filter_btx: bool = False, filter_collada: bool = False, filter_alembic: bool = True, filter_usd: bool = False, filter_obj: bool = False, filter_volume: bool = False, filter_folder: bool = True, filter_blenlib: bool = False, filemode: int = 8, relative_path: bool = True, display_type: str = 'DEFAULT', sort_method: str = '') -> None:

  """

  Add an override layer to the archive

  """

  ...

def layer_move(direction: str = 'UP') -> None:

  """

  Move layer in the list, layers further down the list will overwrite data from the layers higher up

  """

  ...

def layer_remove() -> None:

  """

  Remove an override layer to the archive

  """

  ...

def open(filepath: str = '', hide_props_region: bool = True, filter_blender: bool = False, filter_backup: bool = False, filter_image: bool = False, filter_movie: bool = False, filter_python: bool = False, filter_font: bool = False, filter_sound: bool = False, filter_text: bool = False, filter_archive: bool = False, filter_btx: bool = False, filter_collada: bool = False, filter_alembic: bool = True, filter_usd: bool = False, filter_obj: bool = False, filter_volume: bool = False, filter_folder: bool = True, filter_blenlib: bool = False, filemode: int = 8, relative_path: bool = True, display_type: str = 'DEFAULT', sort_method: str = '') -> None:

  """

  Load a cache file

  """

  ...

def reload() -> None:

  """

  Update objects paths list with new data from the archive

  """

  ...
