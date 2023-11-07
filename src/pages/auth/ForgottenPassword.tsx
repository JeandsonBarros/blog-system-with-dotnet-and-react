import { Button, Input, Spinner, Tab, Tabs } from "@nextui-org/react";
import { Card, CardBody } from "@nextui-org/react";
import { useNavigate } from "react-router-dom";
import InputPassword from "../../components/InputPassword";
import { useContext, useState } from "react";
import { axiosErrorToString } from "../../shared/services/API";
import { changeForgottenPassword, login, sendEmailToForgottenPassword } from "../../shared/services/AuthService";
import { MainContext } from "../../App";

export default function ForgottenPassword() {

    const { setAlert } = useContext(MainContext)
    const navigate = useNavigate()
    const [dataToForgottenPassword, setDataToForgottenPassword] = useState<any>({ code: NaN, email: "", password: "", confirmPassword: "" })
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const [selectedTab, setSelectedTab] = useState<string>("login")

    async function sendEmail(): Promise<void> {

        if (!dataToForgottenPassword.email) {
            return setAlert({
                isVisible: true,
                text: "Enter your email.",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {
            const data = await sendEmailToForgottenPassword(dataToForgottenPassword.email)
            setAlert({ isVisible: true, text: data.message, status: "success" })
            setSelectedTab("change")
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: 'error', isVisible: true })
        }

        setIsLoad(false)
    }

    async function updatePassword(): Promise<void> {

        if (!dataToForgottenPassword.code ||
            !dataToForgottenPassword.password ||
            !dataToForgottenPassword.confirmPassword) {
            return setAlert({
                isVisible: true,
                text: "Don't leave fields empty.",
                status: "warning"
            });
        }

        if (dataToForgottenPassword.password != dataToForgottenPassword.confirmPassword) {
            return setAlert({
                isVisible: true,
                text: "Passwords do not match.",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {

            await changeForgottenPassword(
                dataToForgottenPassword.email,
                dataToForgottenPassword.password,
                Number(dataToForgottenPassword.code)
            )

            await login(dataToForgottenPassword.email, dataToForgottenPassword.password)
            navigate("/")

        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: 'error', isVisible: true })
        }

        setIsLoad(false)
    }

    function setValue(key: string, value: string): void {
        const tempValues = dataToForgottenPassword
        tempValues[key] = value
        setDataToForgottenPassword({ ...tempValues })
    }

    return (
        <section className="flex justify-center items-center h-screen w-full">

            <Card className="max-w-full w-[340px] h-[400px]">

                <CardBody className="overflow-hidden flex items-center h-full">

                    <Tabs
                        aria-label="Options"
                        selectedKey={selectedTab}
                        onSelectionChange={value => setSelectedTab(String(value))}
                    >

                        <Tab
                            title="Send email"
                            key="send"
                        >
                            <form>

                                <Input
                                    variant="underlined"
                                    type="email"
                                    label="Email"
                                    onChange={event => setValue("email", event.target.value)}
                                    description="Enter your email so that a password reset code can be sent to you."
                                />

                                <Button
                                    color="primary"
                                    variant="shadow"
                                    className="w-full mt-3"
                                    onPress={sendEmail}
                                >
                                    {isLoad ? <Spinner color="default" size="sm" /> : <>Send email</>}
                                </Button>

                            </form>

                        </Tab>

                        <Tab
                            key="change"
                            title="Update Password"
                        >

                            <form action="">

                                <Input
                                    type="number"
                                    variant="underlined"
                                    label="Code"
                                    onChange={event => setValue("code", event.target.value)}
                                    description="Enter the code that was sent to your email to confirm."
                                />

                                <InputPassword
                                    label="New password"
                                    setValue={value => setValue("password", value)}
                                />

                                <InputPassword
                                    label="Confirm password"
                                    setValue={value => setValue("confirmPassword", value)}
                                />

                                <Button color="primary" onPress={updatePassword} className="w-full mt-3">
                                    {isLoad ? <Spinner color="default" size="sm" /> : <>Confirm</>}
                                </Button>

                            </form>

                        </Tab>

                    </Tabs >

                </CardBody>

            </Card>

        </section >
    );
}