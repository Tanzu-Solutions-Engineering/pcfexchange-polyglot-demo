#### use this script to create / delete environments for students. must be logged in with admin permissions
Function CreateStudents($students)
{
    For($i=1; $i -lt $students ; $i++){
        $username = "s{0}" -f $i
        $password = "keepitsimple"
        #cf create-org $username
        cf create-user $username $password
        #cf set-org-role $username $username OrgManager
        cf create-space $username -o workshop
        cf set-space-role $username workshop $username SpaceDeveloper
    }
}
Function DeleteStudents($students)
{
    For($i=1; $i -lt $students ; $i++){
        $username = "s{0}" -f $i
        #cf delete-org $username -f
        cf delete-space $username -f
        cf delete-user $username -f
    }
}

$studentCount = 20 # set this to number of students
#CreateStudents -students $studentCount  
#DeleteStudents -students $studentCount  
