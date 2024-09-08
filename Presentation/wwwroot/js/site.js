

document.addEventListener('DOMContentLoaded', function () {
    let usernameInput = document.getElementById('Username');
    let appPasswordInput = document.getElementById('AppPassword');
    let repoUrlInput = document.getElementById('RepoUrl');
    let repoNameInput = document.getElementById('RepoName');
    let gitServiceInput = document.getElementById('GitOnlineService');
    let branchSelect = document.getElementById('BranchName');

    gitServiceInput.addEventListener('change', function () {
        if (gitServiceInput.value == 'github') {
            repoUrlInput.parentElement.classList.remove("hidden");
            repoNameInput.parentElement.classList.add("hidden");
        }
        else if (gitServiceInput.value == 'bitbucket') {
            repoUrlInput.parentElement.classList.add("hidden");
            repoNameInput.parentElement.classList.remove("hidden");
        }
    });

    appPasswordInput.addEventListener('blur', function () {
        getRepositories();
    });

    usernameInput.addEventListener('blur', function () {
        getRepositories();
    });

    function getRepositories() {
        if (appPasswordInput.value && usernameInput.value) {
            const dataWithAuth = {
                username: usernameInput.value,
                appPassword: appPasswordInput.value,
                gitService: gitServiceInput.value
            };

            fetch('/Versions/GetRepositories', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dataWithAuth)
            })
                .then(response => response.json())
                .then(repositories => {
                    repoNameInput.innerHTML = '';

                    repositories.forEach(branch => {
                        const option = document.createElement('option');
                        option.value = branch;
                        option.text = branch;
                        repoNameInput.add(option);
                    });
                })
                .catch(error => console.error('Error fetching repositories:', error));

        }
    }

    repoNameInput.addEventListener('change', function () {
        if (repoNameInput.value) {
            const dataWithAuth = {
                username: usernameInput.value,
                appPassword: appPasswordInput.value,
                repoName: repoNameInput.value,
                gitService: gitServiceInput.value
            };

            fetch('/Versions/GetBranches', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dataWithAuth)
            })
                .then(response => response.json())
                .then(branches => {
                    branchSelect.innerHTML = '';

                    branches.forEach(branch => {
                        const option = document.createElement('option');
                        option.value = branch;
                        option.text = branch;
                        branchSelect.add(option);
                    });
                })
                .catch(error => console.error('Error fetching branches:', error));

        }
    });

    repoUrlInput.addEventListener('blur', function () {
        if (repoUrlInput.value) {
            const dataWithAuth = {
                username: usernameInput.value,
                appPassword: appPasswordInput.value,
                repoUrl: repoUrlInput.value,
                gitService: gitServiceInput.value
            };

            fetch('/Versions/GetBranches', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(dataWithAuth)
            })
                .then(response => response.json())
                .then(branches => {
                    branchSelect.innerHTML = '';

                    branches.forEach(branch => {
                        const option = document.createElement('option');
                        option.value = branch;
                        option.text = branch;
                        branchSelect.add(option);
                    });
                })
                .catch(error => console.error('Error fetching branches:', error));

        }
    });
});
