pipeline {
       agent { label 'app-server' }

    stages {
        stage('Checkout') {
            steps {
                echo 'Pulling latest code from GitHub...'
                checkout scm
            }
        }

        stage('Build') {
            steps {
                echo 'Building .NET application...'
                sh 'dotnet build'
            }
        }

        stage('Check Database Connection') {
            steps {
                echo 'Verifying database connectivity...'
                sh 'curl -f http://localhost:5000/api/dbcheck || echo "DB check needs app running"'
            }
        }

        stage('Pause for Backup') {
            steps {
                echo 'Please back up the existing code on the server before proceeding.'
                input message: 'Have you backed up the current published folder on Server 1?', ok: 'Yes, backup done - proceed to deploy'
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying new code live...'
                sh 'echo "Deploy step - will add actual deploy commands here"'
            }
        }
    }
}
