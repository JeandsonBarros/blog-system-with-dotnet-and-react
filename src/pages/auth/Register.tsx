import { Button, Input, Spinner } from "@nextui-org/react";
import { Card, CardHeader, CardBody, CardFooter } from "@nextui-org/react";
import { Link, useNavigate } from "react-router-dom";
import InputPassword from "../../components/InputPassword";
import { useContext, useState } from "react";
import { axiosErrorToString } from "../../shared/services/API";
import { login, register } from "../../shared/services/AuthService";
import { MainContext } from "../../App";

export default function Register() {

    const { setAlert } = useContext(MainContext)
    const navigate = useNavigate()
    const [user, setUser] = useState<any>({ name: "", password: "", email: "", confirmPassword: "" })
    const [isLoad, setIsLoad] = useState<boolean>(false)
    
    async function userRegister(): Promise<void> {

        if (!user.name || !user.password || !user.email || !user.confirmPassword) {
            return setAlert({
                isVisible: true,
                text: "Don't leave fields empty.",
                status: "warning"
            });
        }

        if (user.password != user.confirmPassword) {
            return setAlert({
                isVisible: true,
                text: "Passwords do not match.",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {
            await register({ name: user.name, password: user.password, email: user.email })
            await login(user.email, user.password)
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
                    <span className="text-default-500 text-lg"> User register </span>
                </CardHeader>

                <CardBody className="py-2">

                    <Input
                        variant="underlined"
                        type="text"
                        label="Name"
                        onValueChange={value => setUser({ ...user, name: value })}
                    />

                    <Input
                        variant="underlined"
                        type="email"
                        label="Email"
                        onValueChange={value => setUser({ ...user, email: value })}
                    />

                    <InputPassword
                        label="Password"
                        setValue={value => setUser({ ...user, password: value })}
                    />

                    <InputPassword
                        label="Confirm password"
                        setValue={value => setUser({ ...user, confirmPassword: value })}
                    />

                </CardBody>

                <CardFooter className="flex flex-col">

                    <Button
                        color="primary"
                        variant="shadow"
                        className="w-full"
                        onPress={userRegister}
                    >
                        {isLoad ? <Spinner color="default" size="sm" /> : <>Register</>}
                    </Button>

                    <hr className="w-full m-4" />

                    <Link to="/login" className="linkCustom">Login</Link>

                </CardFooter>

            </Card>

        </section>
    );
}