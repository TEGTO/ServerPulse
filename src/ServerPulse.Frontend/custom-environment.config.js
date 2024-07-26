const fs = require('fs');
const path = require('path');

const apiUrl = process.env.API_URL || 'https://localhost:7129';

const replaceInFile = (filePath, param, value) => {
    const fileContent = fs.readFileSync(filePath, 'utf8');
    const updatedContent = fileContent.replace(new RegExp(`${param}: .*`, 'g'), `${param}: '${value}',`);
    fs.writeFileSync(filePath, updatedContent, 'utf8');
    console.log(`Replaced ${param} in ${filePath}`);
};

const environmentFiles = [
    path.join(__dirname, 'src/environment/environment.ts'),
    path.join(__dirname, 'src/environment/environment.prod.ts')
];

environmentFiles.forEach(filePath => {
    replaceInFile(filePath, 'api', apiUrl);
});
