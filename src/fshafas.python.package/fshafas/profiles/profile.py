from ..fable_modules.fs_hafas_profiles_python.db.profile import profile as db_internal_profile
from ..fable_modules.fs_hafas_profiles_python.oebb.profile import profile as oebb_internal_profile
from ..fable_modules.fs_hafas_api_python.types_hafas_client import Profile

db_profile: Profile = db_internal_profile

oebb_profile: Profile = oebb_internal_profile
