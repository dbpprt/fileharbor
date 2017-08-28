$apiUrl = "http://localhost:8080"

$collectionTemplateId = "481e6c51-45d3-4fdc-a2a4-90636a5de72d"

$email = (-join ((65..90) + (97..122) | Get-Random -Count 5 | % {[char]$_}))
$email  += "@mail.com"
$password = (-join ((65..90) + (97..122) | Get-Random -Count 10 | % {[char]$_}))
$givenname = (-join ((65..90) + (97..122) | Get-Random -Count 10 | % {[char]$_}))
$collectionName = (-join ((65..90) + (97..122) | Get-Random -Count 10 | % {[char]$_}))

# register a new user
$json = @{
    givenname= $givenname
    email= $email
    password = $password
} | ConvertTo-Json

$response = Invoke-RestMethod "$apiUrl/users/register" -Method Post -Body $json -ContentType 'application/json'

if ([Guid]::TryParse($response.id, [ref][Guid]::Empty) -eq $true) {
    Write-Warning "created a new user $email with password $password"
} else {
    Write-Error "user creation failed :("
    return
}

$json = @{
    email= $email
    password = $password
} | ConvertTo-Json

$response = Invoke-RestMethod "$apiUrl/users/login" -Method Post -Body $json -ContentType 'application/json' -ErrorAction Stop
$token = $response.token
$headers = @{"Authorization" = "Bearer " + $token}

Write-Warning "logged in and obtained token $token"

$response = Invoke-RestMethod "$apiUrl/collections/my" -Method Get -ContentType 'application/json' -Headers $headers -ErrorAction Stop
$collectionId = $response[0].id

Write-Warning "default collection id is $($response[0].id)"

$response = Invoke-RestMethod "$apiUrl/collections/templates" -Method Get -ContentType 'application/json' -Headers $headers -ErrorAction Stop

$response | % { Write-Warning "found template id: $($_.id), name: $($_.name), language: $($_.language)" }

$json = @{
    id= $collectionId
    name= "my collection"
    template_id = $collectionTemplateId
} | ConvertTo-Json

$response = Invoke-RestMethod "$apiUrl/collections/initialize" -Method Post -Body $json -ContentType 'application/json' -Headers $headers -ErrorAction Stop

Write-Warning "initialized collection $collectionId with template $collectionTemplateId"

$response = Invoke-RestMethod "$apiUrl/collections/my" -Method Get -ContentType 'application/json' -Headers $headers -ErrorAction Stop

if ($response[0].template_id -eq $collectionTemplateId) {
    Write-Warning "initialization successfully, rechecked template_id"
} else {
    Write-Error "unknown initialization error - double check failed"
}

$json = @{
    id= $collectionId
    name= $collectionName
} | ConvertTo-Json

$response = Invoke-RestMethod "$apiUrl/collections/name" -Method Post -Body $json -ContentType 'application/json' -Headers $headers -ErrorAction Stop

$response = Invoke-RestMethod "$apiUrl/collections/my" -Method Get -ContentType 'application/json' -Headers $headers -ErrorAction Stop

if ($response[0].name -eq $collectionName) {
    Write-Warning "name update successfully, rechecked name"
} else {
    Write-Error "unknown name update error - double check failed"
}
