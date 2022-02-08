from setuptools import setup

# todo: fable_simple_json_python, fable_python and fable_library as requirements

setup(
    name='fshafas',
    version='0.0.4',
    description='python client for hafas endpoint api',
    url='https://github.com/bergmannjg/fshafas',
    license='MIT',
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
        'polyline'
    ],
    python_requires='>=3.8',
)
