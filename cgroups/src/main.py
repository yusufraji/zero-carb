import subprocess
import time
from getpass import getpass

import click
from process_util import get_genesis


def set_cgroups(sudo_password):
    genesis_dict = get_genesis()
    for item in genesis_dict:
        user = item["user"].replace("(", "").replace(")", "")
        pid = item["pid"]
        commands = f"sudo cgcreate -g perf_event:{user};\
            sudo cgcreate -g perf_event:{user};\
                sudo cgclassify -g perf_event:{user} {pid}"
        kwargs = dict(
            stdout=subprocess.PIPE, encoding="ascii", input=sudo_password, shell=True
        )
        try:
            result = subprocess.run(commands, **kwargs)
        except subprocess.CalledProcessError as e:
            print(e)


@click.command()
@click.option(
    "--interval",
    default=1,
    help="interaval to obtain PID, equivalent to sensor interval & smartwatts-formula interval (ms)",
)
def main(interval):
    sudo_password = getpass(prompt="sudo password: ")
    while True:
        start_time = time.time()

        set_cgroups(sudo_password)

        elapsed_time = (time.time() - start_time) * 1000
        if elapsed_time < interval:
            time.sleep(interval - elapsed_time)


if __name__ == "__main__":

    main()
