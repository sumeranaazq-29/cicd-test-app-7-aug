pipeline {
    agent { label 'app-server' }

    environment {
        STAGING = '/home/ubuntu/jenkins-agent/publish-staging'
        LIVE    = '/home/ubuntu/deployed/testapi'
        BACKUPS = '/home/ubuntu/backups'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Pulling latest code from GitHub...'
                checkout scm
            }
        }

        stage('Build') {
            steps {
                echo 'Building and publishing .NET application...'
                sh '''
                    rm -rf "$STAGING"
                    dotnet publish -c Release -o "$STAGING"
                '''
            }
        }

        stage('Approval') {
            steps {
                echo 'Waiting for approval to deploy...'
                timeout(time: 30, unit: 'MINUTES') {
                    input message: 'Deploy the new build to the live app? (An automatic backup is taken first.)', ok: 'Yes, deploy'
                }
            }
        }

        stage('Deploy') {
            steps {
                echo 'Backing up the live folder, then deploying...'
                sh '''
                    set -e
                    mkdir -p "$BACKUPS"
                    tar -czf "$BACKUPS/testapi-$(date +%F-%H%M%S).tgz" -C "$(dirname "$LIVE")" "$(basename "$LIVE")"
                    sudo systemctl stop testapi
                    rm -rf "$LIVE"/*
                    cp -r "$STAGING"/* "$LIVE"/
                    sudo systemctl start testapi
                '''
            }
        }

        stage('Verify Deployment') {
            steps {
                echo 'Verifying the new version is running and can reach the database...'
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
    }

    post {
        success {
            echo 'Pipeline completed successfully!'
        }
        failure {
            echo 'Pipeline failed. Check the logs above. Latest backup is in /home/ubuntu/backups.'
        }
    }
}
