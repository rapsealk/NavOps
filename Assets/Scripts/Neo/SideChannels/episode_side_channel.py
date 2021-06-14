#!/usr/bin/python3
# -*- coding: utf-8 -*-
import uuid

import numpy as np
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.side_channel.side_channel import SideChannel, IncomingMessage


class EpisodeSideChannel(SideChannel):

    def __init__(self):
        super().__init__(uuid.UUID("0a6d3d29-0130-475c-a98c-ae665a752cbc"))

    def on_message_received(self, msg: IncomingMessage):
        print(msg.read_string())


if __name__ == "__main__":
    pass
