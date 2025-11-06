import os
import subprocess
import platform
import json

USERS_JSON_PATH = os.path.join(os.path.dirname(__file__), 'users.json')

USERNAME_KEY = 'Username'

def get_machine_id() -> str:
    """
    Gets the current user's machine ID.

    Returns:
         The unique machine identifier of type string.
    """

    system = platform.system()

    if system == 'Windows':
        cmd = "wmic csproduct get uuid"
        result = subprocess.check_output(cmd, shell=True).decode()
        lines = result.splitlines()
        uuid = lines[2]

    else:
        raise Exception

    return uuid


def get_users() -> dict:
    """ Retrieves information about users and their info.

    Returns:
        dict: A dictionary containing users and their info {user, user_info}.
    """

    if os.path.exists(USERS_JSON_PATH) == False:
        raise Exception('Users JSON file cannot be found.')

    with open(USERS_JSON_PATH, 'r') as f:
        users = json.load(f)
        return users


def add_user(machine_id : str):
    """ Add a user to the users JSON file.

    Args:
        machine_id (str): The unique machine identifier.
    """

    # Create the user's user info
    user_info = {
        USERNAME_KEY: ''
    }

    users = get_users()
    users[machine_id] = user_info

    # Save the new information into the JSON file
    with open(USERS_JSON_PATH, 'w') as file:
        json.dump(users, file, indent=4)

def set_username(machine_id : str, name : str):
    """ Set the user's username in the users JSON file.

    Args:
        machine_id (str): The unique machine identifier of the user.
        name (str): The name of the user.
    """

    users = get_users()
    user_info = users[machine_id]
    user_info[USERNAME_KEY] = name

    # Save the new information into the JSON file
    with open(USERS_JSON_PATH, 'w') as file:
        json.dump(users, file, indent=4)

def set_user_attr(machine_id : str, attr : str, value : str):
    """ Set the user's username in the users JSON file.

    Args:
        machine_id (str): The unique machine identifier of the user.
        attr (str): The name of the attribute.
        value (str): The value of the attribute.
    """

    users = get_users()
    user_info = users[machine_id]
    user_info[attr] = value

    # Save the new information into the JSON file
    with open(USERS_JSON_PATH, 'w') as file:
        json.dump(users, file, indent=4)