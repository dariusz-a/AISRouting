const express = require('express');
const path = require('path');

const app = express();
const port = process.env.PORT || 5173;

app.use(express.static(path.join(__dirname, 'app')));

app.get('/', (req, res) => res.sendFile(path.join(__dirname, 'app', 'index.html')));

app.listen(port, () => console.log(`Static server running at http://localhost:${port}`));
