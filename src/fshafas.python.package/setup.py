from setuptools import setup
import pathlib
from setuptools import setup

setup(
    name='fshafas',
    version='0.0.22',
    description='python client for HAFAS public transport APIs',
    long_description=open('package.readme.md').read(),
    long_description_content_type="text/markdown",
    url='https://github.com/bergmannjg/fshafas',
    license='MIT',
    author="Jürgen Bergmann",
    author_email="jbergmann@posteo.de",
    classifiers=[
        'Development Status :: 4 - Beta',
        'Intended Audience :: Developers',
        'Programming Language :: Python :: 3.8',
    ],
    packages=[
        'fshafas',
        'fshafas/profiles',
        'fshafas/fable_modules/fs_hafas_python',
        'fshafas/fable_modules/fs_hafas_python/format',
        'fshafas/fable_modules/fs_hafas_python/parse',
        'fshafas/fable_modules/fs_hafas_python/lib',
        'fshafas/fable_modules/fs_hafas_python/extensions',
        'fshafas/fable_modules/fs_hafas_profiles_python/db',
        'fshafas/fable_modules/fs_hafas_profiles_python/bvg',
        'fshafas/fable_modules/fs_hafas_profiles_python/oebb',
        'fshafas/fable_modules/fs_hafas_profiles_python/rejseplanen',
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
