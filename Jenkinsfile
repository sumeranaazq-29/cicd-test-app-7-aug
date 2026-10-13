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
                sh '''
                    for i in 1 2 3 4 5; do
                        if curl -fsS --max-time 5 http://localhost:5000/api/dbcheck; then
                            echo "DB check passed"
                            exit 0
                        fi
                        echo "Attempt $i failed, retrying in 5 seconds..."
                        sleep 5
                    done
                    echo "DB check FAILED: app or database not reachable"
                    exit 1
                '''
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
