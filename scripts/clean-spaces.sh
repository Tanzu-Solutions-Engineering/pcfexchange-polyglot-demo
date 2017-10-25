#!/bin/bash
#
# This script takes in a list of emails (newline seperated), deletes, and recreatescf spaces in the
# fintech org.  Each email address will be used as a space name.
# In addition, the "student" quota will be assigned to the space to restrict usage.
#

if [ -z "$1" ]; then
  echo usage: $0 \<filename containing emails - 1 email per line\>
  exit
fi

filename=$1

org="fintech"
org_guid=$(cf org $org --guid)
echo "INFO: guid for $org is $org_guid"

for email in $(cat $filename) ; do

	echo "INFO: Deleting space: $email"
        cf delete-space $email -o $org -f

	echo "INFO: Creating space $email"
	cf create-space $email -o $org

#	Attempt to add the user to the organization
#	cf curl "/v2/organizations/$org_guid/users" -X PUT -d "{\"username\": \"$email\"}"

	cf set-space-role $email $org $email SpaceDeveloper
  cf set-space-quota $email student

done
