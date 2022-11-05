import getpass
from pprint import pprint

import psutil


def get_genesis():
    genesis = list()

    for process in psutil.process_iter(["name", "username"]):

        _genesis = dict()
        # The processes with 0 parents are "systemd" and "kthreadd" owned
        # by root.
        # The next processes owned by the user will have at least one
        # parent, of these processes, the ones with only one parent are
        # those owned by root ("systemd" and "<gui-desktop>",
        # "gnome-keyring-daemon" in my case).
        # So, to find the genesis processes of other programs, i'm filtering
        # by processes with exactly 2 parents and belonging to the current user.

        if (process.info["username"] == getpass.getuser()) and (
            len(process.parents()) == 2
        ):
            _genesis["pid"] = process.pid
            _genesis["user"] = process.name()
            _genesis["username"] = process.username()
            genesis.append(_genesis)

    return genesis


if __name__ == "__main__":
    pprint(get_genesis())
