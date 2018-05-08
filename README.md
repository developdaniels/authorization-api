# authorization-api
Challenge Back-End for Genesis Automation in Healthcare

Please visit https://gitlab.com/developdaniels/authorization-api/activity to see all my commits descriptions...

Start using acessing http://localhost:50123/swagger/ or calling the following endpoints:

POST http://localhost:50123/api/Users/SignUp

POST http://localhost:50123/api/Users/SignIn

GET http://localhost:50123/api/Users/Search/a292fcfa-41c0-4a71-b854-53481f52d4bb

# Examples

# POST SignUp_Valid
  curl -X POST \
    http://localhost:50123/api/Users/SignUp \
    -H 'Cache-Control: no-cache' \
    -H 'Content-Type: application/json' \
    -H 'Postman-Token: d32762d5-0bc2-4117-8dc7-ba0ae8947cbc' \
    -d '{
    "name": "Full Name",
    "email": "mail@domain.com",
    "password": "passsword",
    "telephones": [{"number":"5123456789"}]
  }'

# POST SignIn_Valid
  curl -X POST \
    http://localhost:50123/api/Users/SignIn \
    -H 'Cache-Control: no-cache' \
    -H 'Content-Type: application/json' \
    -H 'Postman-Token: e3d434b7-bbef-4f25-9632-83f426fe6812' \
    -d '{
    "email": "mail@domain.com",
    "password": "passsword"
  }'

# POST Search_Valid
  curl -X GET \
    http://localhost:50123/api/Users/Search/a292fcfa-41c0-4a71-b854-53481f52d4bb \
    -H 'Authentication: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6ImEyOTJmY2ZhLTQxYzAtNGE3MS1iODU0LTUzNDgxZjUyZDRiYiIsImxhc3RMb2dpbk9uIjoiNS84LzIwMTggMTI6MTI6NDUgQU0ifQ.rNtOFvuPhmrAubCd6HUDPdSc2VoTJhVqjfkrMWr6FlU' \
    -H 'Cache-Control: no-cache' \
    -H 'Postman-Token: 06b6d876-cb0d-4ae5-96b0-cbf391d8119b' \
    -d '{
    "email": "developdaniels@gmail.com",
    "password": "pijama"
  }'
