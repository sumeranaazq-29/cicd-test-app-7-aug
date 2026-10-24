# Jenkins CI/CD Pipeline on AWS EC2

A working CI/CD pipeline that builds, tests, backs up and deploys a .NET 8 + React + PostgreSQL application across two AWS EC2 servers using Jenkins. Every push to GitHub triggers a build automatically, and a manual approval gate protects the deploy step.

![Pipeline status](docs/screenshots/pipeline-green.png)
<!-- Add a screenshot of your green Jenkins build here -->

## What this project shows

- Automated builds triggered by a GitHub webhook
- A Jenkins agent connected over SSH, so the build runs on the application server
- A multi-stage Jenkinsfile with an approval gate, automatic backup, deploy and a retrying database health check after deployment
- The API running as a `systemd` service with automatic restart
- Real troubleshooting notes from problems hit along the way

## Architecture

```
Developer
   |  git push
   v
GitHub repository
   |  webhook (port 8080)
   v
DevOps Server (Ubuntu, EC2)
   Jenkins controller
   |  SSH agent connection
   v
App Server (Ubuntu, EC2)
   .NET 8 Web API  +  React (Vite)  +  PostgreSQL
   systemd service: testapi
```

| Server | Role | Software |
| --- | --- | --- |
| DevOps Server | CI controller | Jenkins 2.568.2, Java 21 |
| App Server | Build agent and application host | .NET 8 SDK, Node.js, PostgreSQL |

Both servers are Ubuntu `t3.micro` instances in the `ap-south-1` region.

## Pipeline stages

| Stage | What it does |
| --- | --- |
| Checkout | Pulls the latest code from GitHub |
| Build | Runs `dotnet publish -c Release` on the App Server agent into a staging folder |
| Approval | Pauses for a person to approve the deployment, with a 30-minute timeout |
| Deploy | Takes a dated `.tgz` backup of the live folder, stops the `testapi` service, replaces the files and starts the service again |
| Verify Deployment | Calls `/api/dbcheck` up to 5 times, 5 seconds apart, and fails the build if the app or database is unreachable |
| Post Actions | Prints success, or on failure points to the latest backup in the backups folder |

## Tech stack

- **CI/CD:** Jenkins, GitHub, webhooks, Jenkinsfile (declarative pipeline)
- **Cloud:** AWS EC2, security groups
- **Backend:** .NET 8 Web API
- **Frontend:** React with Vite, served from the API's `wwwroot`
- **Database:** PostgreSQL
- **OS and services:** Ubuntu, systemd, SSH

## How to set it up

1. **Launch two Ubuntu EC2 instances** and open only the ports you need in the security groups: SSH (22) from your own IP, Jenkins (8080) from your IP and GitHub's webhook ranges.
2. **App Server:** install the .NET 8 SDK, Node.js and PostgreSQL. Create the database and run the API once to confirm it connects.
3. **DevOps Server:** install Java 21 and Jenkins. Complete the Jenkins setup wizard and install the suggested plugins.
4. **Connect the agent:** in Jenkins, add the App Server as an SSH agent with the label `app-server`, using an SSH credential stored in Jenkins credentials.
5. **Create the pipeline job** pointing at this repository, and set the Jenkinsfile to use `agent { label 'app-server' }`.
6. **Add the webhook:** in GitHub repository settings, add `http://<devops-server-ip>:8080/github-webhook/` and enable "push" events.
7. **Run it:** push a commit and watch the build start by itself.

## Problems I solved

| Problem | Cause | Fix |
| --- | --- | --- |
| Plugins failed to load | Jenkins version needed a newer Java | Upgraded to Java 21 and a newer Jenkins release |
| Build node went offline | `/tmp` had too little free space | Freed space and brought the node back online |
| `dotnet: not found` during build | Build was running on the controller, which has no .NET | Added the App Server as an SSH agent and used its label in the Jenkinsfile |
| Webhook stopped working | Public IPs changed after a server restart | Updated the webhook URL and security group rules |
| Jenkins repo key errors | Repository GPG signing problem | Installed Jenkins from the direct `.deb` package |

## Security notes

- Never commit `.pem` keys, passwords or connection strings. Use Jenkins credentials and environment variables.
- Restrict SSH to your own IP instead of `0.0.0.0/0`.
- The approval stage exists so a human checks every release before it reaches the live service.

## Roadmap

- Automatic rollback to the last backup if verification fails (today the failed build message points to the backup, and the restore is manual)
- Security hardening: restricted sudo, builds off the controller, HTTPS
- Static analysis (SonarQube) and dynamic scanning (OWASP ZAP) stages
- Notifications for every stage

## Author

**Sumera Naaz Qureshi** — Cloud & DevOps Intern
GitHub: [sumeranaazq-29](https://github.com/sumeranaazq-29)
