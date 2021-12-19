import React from 'react';
import { render } from 'react-dom';
import { BrowserRouter as Router } from "react-router-dom";
import App from './Components/AppComponent';
import '../scss/main.scss';

render(
    <React.StrictMode>
        <Router basename='/app'>
            <App />
        </Router>
    </React.StrictMode>,
    document.getElementById('root')
)