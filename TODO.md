# App Improvements

## Task based
- [ ] Create categories and labels
- [x] Implement pagination and sorting (Get all tasks?)
- [x] Implement searching through keywords... from title and description.
- [ ] Feature to add attachments in tasks (1mb)... (images, pdf and define what types are allowed) paid members have higher 10mb upload limit.
    - [ ] change names of the file uploaded to the databse to ensure obscurity
    - [ ] make sure to check inner file headers to ensure some sketchy file with changed ext is not being uploaded

## User based
- [x] learn about using jwt token with refresh tokens?
- [x] Add (MANUAL) role-based authentication
- [x] add avatars; static files
- [ ] Validate Email Address using sendgrid api?

- [ ] add oauth or signin with google shit. also look into OpenID Connect
- [ ] Implement auth with APIKey too? Take the apikey within headers. And then there is session based auth in .net
- [x] WHEN USERNAME CHANGES... LOGOUT
- [x] Implement logout functionality; MAKE JWT STORE IN DATABASE;
- [x] Hash the passwords and salt them and implement password policy

## Misc
- [ ] Add a tests project do testing
- [x] Set and secure appsettings with user secrets in aspnetcore.
- [x] CREATE DTOs
- [x] CONNECT USER AND TASKS
- [ ] Exception Handling
    - [ ] Update controller and repo to catch and throw errors.
    - [ ] ModelState.IsValid (tells if field attributes were validated or not. If [EmailAddress] attribute fails to validate the email this IsValid will return false)
- [ ] set versioning in api
- [x] Time is an issue (UTC is getting stored in database) http://worldtimeapi.org/
    - [x] A Timezone property to accordingly change user time (due, creation, modified time) in frontend

- [ ] Think of rate limiting?
- [ ] 2 factor auth
- [ ] Cache storage?
- [ ] Payment service?
- [ ] Think of giving other users editor and viewer access to an owned task??

## Minor changes
ALL DONE
- [ ] Rename controllers and stuff at the very final.
- [x] What is  Guid.NewGuide()?
- [ ] Response class model with properties Status(success, fail, redirect or whatever) and Message (user created, deleted and whatever);
- [ ] Checkout usermanager, rolemanager?

## Others
- Upload the project to RapidAPI.
- Streaming Availabiltiy App which searches the a particular movie or show for availability on which ott.
- Make a solution with different projects... Model project (includes dto), Services project(includes db context), Authentication project 
- URL Shortner app is a simple app. Easy basics... can do a lot for upgrading it with good features.