import { Link } from "react-router-dom";

function Error404() {
    return (
        <section className="h-screen w-screen justify-center flex flex-col items-center">
            <h1>Error 404</h1>
            <h2>Page not found</h2>
            <Link to="/">Back to home</Link>
        </section>
    );
}

export default Error404;