- Don't work with unique ids but primary ids... just make sure to use DTOs so that you can hide the database identity of a particular record.
- Use Guid to make unique IDs but keep the Id as string.

- Searching - search within description and title
- Filtering - isComplete? isModified? isDue? created after / before, modified after / before, due after / before
- Pagination - step 3 page 5
- Sorting (Creation Date, Modified Date, Due date, Name) | (asc / desc)

# Optionals
- You can define policies in JWT!! (This will make the authentication token a lot longer...) By adding different claims. Example: `new Claim("viewTags", true)`
- Policy is like a user... Role is like a group... policy can be part of a role(hypothetically; could be in actual too?)
- You can use rolemanager to add a particular role to the user(don't even need to use a Role property);
    - A role is a business level of representation; it is just a bunch of permissions grouped together.
- Adding these roles and policies does add more to your migration `;)` ie new tables

- A movie database - 18- getting restricted policy from viewing A rated films
- After policies / roles, we have authorization handlers... Say when a user registers, or logins... The authorization handler automatically assigns the user a certain role based on a given criteria (a user has completed more than 6 months in the course, unlock labs; a user email address in a particular domain name... @capgemini.com and the website authenticating on is also capgemini.com so the user is automatically allocated the employee role)

URL shortner
 
stores url mappings into the database
    - A user has only 50 url short forms in free tier and URLs expire at after 1 year
    - A premium user has unlimited url short forms and never expiring url
- Guest users can store 5 urls (Then, block by IP);
Operations
 
 
URL Properties:
    - Guid Id
    - Original URL
    - ShortURL
    - Clicks
    - Created at
    - Expiration?
 
User Properties
    - Guid Id
    - Username
    - FirstName
    - LastName
    - Email
    - Password
    - Tokens / Try implementing OpenID or OAuth
    - Avatar
    - Timezone
