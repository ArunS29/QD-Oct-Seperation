Start-Transcript -Path "F:\CICD\qd.erp.web\logs\log.txt"
# Define variables
# $repoPath = "C:\path\to\your\repo"
$sshKeyPath = "C:\\ProgramData\\ssh\\id_rsa"
$publishProfilePath = "F:\\CICD\\qd.erp.web\\QDERPWeb\\Properties\\PublishProfiles\\FolderProfile.pubxml"

# Change to the repository directory
# Set-Location $repoPath

try{
	# Set the SSH key
	$env:GIT_SSH_COMMAND = "ssh -i $sshKeyPath"
	
	# Pull the latest changes from the repository
	git pull --force

	# Restore the .NET Core project
	dotnet restore

	cd QDERPWeb
	# Build the .NET Core project
	# dotnet build

	
	Stop-Service -Name 'W3SVC'


	# Publish the .NET Core project using the publish profile
	dotnet publish QD.ERP.Web.csproj -p:PublishDir=C:\inetpub\wwwroot\erp\ -p:PublishProfileFullPath=$publishProfilePath
	#-p:PublishProfileFullPath=$publishProfilePath
	Start-Service -Name 'W3SVC'

} catch {
    Write-Error $_.Exception.Message
} finally {
    Stop-Transcript
    exit
}