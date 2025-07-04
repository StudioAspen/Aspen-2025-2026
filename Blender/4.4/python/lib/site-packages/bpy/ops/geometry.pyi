"""


Geometry Operators
******************

:func:`attribute_add`

:func:`attribute_convert`

:func:`attribute_remove`

"""

import typing

def attribute_add(name: str = 'Attribute', domain: str = 'POINT', data_type: str = 'FLOAT') -> None:

  """

  Add attribute to geometry

  """

  ...

def attribute_convert(mode: str = 'GENERIC', domain: str = 'POINT', data_type: str = 'FLOAT') -> None:

  """

  Change how the attribute is stored

  """

  ...

def attribute_remove() -> None:

  """

  Remove attribute from geometry

  """

  ...
