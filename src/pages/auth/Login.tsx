import { Button, Card, CardBody, CardFooter, CardHeader, Input, Spinner } from '@nextui-org/react';
import { useContext, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

import { MainContext } from '../../App';
import InputPassword from '../../components/InputPassword';
import { axiosErrorToString } from '../../shared/services/API';
import { login } from '../../shared/services/AuthService';

function Login() {

    const { setAlert } = useContext(MainContext)
    const navigate = useNavigate()
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();
    const [isLoad, setIsLoad] = useState<boolean>(false)

    async function loginUser(): Promise<void> {

        if (!email || !password) {
            return setAlert({
                isVisible: true,
                text: "Don't leave fields empty!",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {
            await login(email, password)
            navigate("/")
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: 'error', isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <section className="flex justify-center items-center h-screen">

            <Card className="py-4 w-96">

                <CardHeader className="pb-0 pt-2 px-4 flex-col items-start">
                    <span className="text-default-500 text-lg"> Login</span>
                </CardHeader>

                <CardBody className="py-2">

                    <Input
                        variant="underlined"
                        type="email"
                        label="Email"
                        onChange={value => setEmail(value.target.value)}
                    />

                    <InputPassword
                        label="Password"
                        setValue={value => setPassword(value)}
                    />
                    <Link to="/forgotten-password" className="linkCustom">
                        <small>Forgot password</small>
                    </Link>

                </CardBody>

                <CardFooter className="flex flex-col">

                    <Button
                        color="primary"
                        variant="shadow"
                        className="w-full"
                        onClick={loginUser}
                    >
                        {isLoad ? <Spinner color="default" size="sm" /> : <>Login</>}
                    </Button>

                    <hr className="w-full m-4" />

                    <Link to="/register" className="linkCustom">Register</Link>

                </CardFooter>

            </Card>

        </section>
    );
}

export default Login;