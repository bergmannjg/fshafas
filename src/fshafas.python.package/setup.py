from setuptools import setup
import pathlib
from setuptools import setup

this_directory = pathlib.Path(__file__).parent
README = (this_directory / "README.md").read_text()

setup(
    name='fshafas',
    version='0.0.5',
    description='python client for HAFAS public transport APIs',
    long_description=README,
    long_description_content_type="text/markdown",
    url='https://github.com/bergmannjg/fshafas',
    license='MIT',
    author="Jürgen Bergmann",
    author_email="jbergmann@posteo.de",
    classifiers=[
        'Development Status :: 3 - Alpha',
        'Intended Audience :: Developers',
        'Programming Language :: Python :: 3.8',
    ],
    packages=[
        'fshafas',
        'fshafas/fable_modules/fs_hafas_python',
        'fshafas/fable_modules/fs_hafas_python/format',
        'fshafas/fable_modules/fs_hafas_python/parse',
        'fshafas/fable_modules/fs_hafas_python/lib',
        'fshafas/fable_modules/fs_hafas_profiles_python/db',
        'fshafas/fable_modules/fable_simple_json_python',
        'fshafas/fable_modules/fable_python/stdlib',
        'fshafas/fable_modules/fable_library',
    ],
    install_requires=[
        'requests',
        'python-slugify',
        'polyline',
        'jsonpickle'
    ],
    python_requires='>=3.8',
)
