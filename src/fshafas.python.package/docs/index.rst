============================
FsHafas-Python documentation
============================

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   fshafas
   fshafas.profiles
   fshafas.fable_modules.fs_hafas_python

FsHafas-Python is a client for the `HAFAS <https://de.wikipedia.org/wiki/HAFAS>`_ public transport APIs.
It is generated (via `Fable <https://github.com/fable-compiler/Fable>`_) 
by the `fshafas <https://github.com/bergmannjg/fshafas/>`_ F# library.

Example
-------

Get journeys between locations and print start station, end station and departure time.

.. code:: Python

    import asyncio
    import sys
    from fshafas import HafasClient
    from fshafas.profiles import db_profile

    async def main(argv) -> int:
        with HafasClient(db_profile) as client:
            journeys = await client.journeys(argv[0], argv[1])
            for j in journeys.journeys:
                for l in j.legs:
                    print(l.origin.fields[0].name, l.destination.fields[0].name, l.departure)
        return 0

    if __name__ == "__main__":
        if len(sys.argv) == 3:
            asyncio.run(main(sys.argv[1:]))

.. code:: shell

    python3.10 example.py Hamburg Berlin

Installation
------------

Replace x.y.z with current `release <https://github.com/bergmannjg/fshafas/releases>`.

.. code:: shell

    pip install https://github.com/bergmannjg/fshafas/releases/download/x.y.z/fshafas-0.0.xy-py3-none-any.whl