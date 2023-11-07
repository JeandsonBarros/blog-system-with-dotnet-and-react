import { Button, ButtonGroup, Card, CardBody, Input, Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, Progress, Spinner } from "@nextui-org/react";
import { useContext, useEffect, useState } from "react";
import { deleteAccount, getDataAccount, getProfileImage, patchAccount, userDeleteProfilePhoto } from "../../shared/services/AuthService";
import { axiosErrorToString } from "../../shared/services/API";
import InputPassword from "../../components/InputPassword";
import AccountConfigStyles from "../../styles/pages_styles/auth_styles/AccountConfig.module.css";
import PersonCircle from "../../assets/img/person-circle.svg";
import User from "../../shared/models/User";
import { useNavigate } from "react-router-dom";
import { MainContext } from "../../App";
import { MdDelete, MdSave } from "react-icons/md";
import axios from "axios";

export default function AccountConfig() {

    const [user, setUser] = useState<User>()
    const { setAlert } = useContext(MainContext)
    const [isLoad, setIsLoad] = useState<boolean>(true)

    useEffect(() => {
        getDataAccount()
            .then(response => {
                setUser(response.data)
                setIsLoad(false)
            })
            .catch(error => {
                setAlert({ text: axiosErrorToString(error), status: 'error', isVisible: true })
                setIsLoad(false)
            })
    }, [])

    return (
        <section>

            {isLoad &&
                <Progress
                    size="sm"
                    isIndeterminate
                    aria-label="Loading..."
                    className="w-full"
                />
            }

            <h1 className="text-lg m-3">Account config</h1>

            <hr />

            <div className="flex flex-col" style={{ maxWidth: 600 }}>

                <UserProfileImage fileProfilePictureName={user?.fileProfilePictureName} />
                <UserName name={user?.name || ""} />
                <UserEmail email={user?.email || ""} />
                <UserPassword />
                <UserRomoverAccount />

            </div>

        </section>
    );
}

function UserProfileImage({ fileProfilePictureName }: { fileProfilePictureName?: string }) {

    const [isImageSaved, setIsImageSaved] = useState<boolean>(false)
    const [previewImage, setPreviewImage] = useState<string>(PersonCircle)
    const [fileImage, setFileImage] = useState<File | undefined>()
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    useEffect(() => {
        if (fileProfilePictureName) {
            setIsImageSaved(true)
            getProfileImage(fileProfilePictureName)
                .then(file => {
                    setFileImage(file)
                    setPreviewImage(URL.createObjectURL(file))
                })
        }
    }, [fileProfilePictureName])

    async function updateImg(): Promise<void> {

        setIsLoad(true)

        try {
            await patchAccount({ fileProfilePicture: fileImage })
            setIsImageSaved(true)
            setAlert({ text: "Profile picture updated successfully.", status: "success", isVisible: true })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    function removeProfilePhoto(): void {

        try {

            if (isImageSaved) {
                userDeleteProfilePhoto()
                setIsImageSaved(false)
            }

            setFileImage(undefined)
            setPreviewImage(PersonCircle)

        } catch (error) {
            if (axios.isAxiosError(error) && error.status != 404) {
                setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
            }
        }

    }

    return (
        <>
            <Card className="m-3">
                <CardBody >
                    <label className="text-default-500 text-lg"> Profile picture </label>

                    <div className="flex flex-col items-center">

                        <div className="flex flex-col border border-gray-300 items-center drop-shadow-2xl p-2 m-2 rounded-lg">

                            <img
                                className={AccountConfigStyles.userPicture}
                                src={previewImage}
                            />

                            <label htmlFor="profileImage" className="bg-gray-500/100 cursor-pointer rounded-lg text-white p-2">
                                Select an image
                            </label>

                        </div>

                        <input
                            id="profileImage"
                            type="file"
                            className="hidden"
                            onChange={event => {

                                if (!event.target.files || event.target.files.length === 0) {
                                    setFileImage(undefined)
                                    setPreviewImage(PersonCircle)
                                    return
                                }

                                setFileImage(event.target.files[0])
                                setPreviewImage(URL.createObjectURL(event.target.files[0]))

                                event.target.value = "";
                            }}
                        />

                        <ButtonGroup>

                            <Button
                                className="w-28"
                                onPress={updateImg}
                                color="primary"
                                isDisabled={!fileImage}
                                title="Save editions"
                            >
                                {isLoad
                                    ? <Spinner color="default" size="sm" />
                                    : <> Save <MdSave /> </>
                                }
                            </Button>

                            <Button
                                className="w-28"
                                onPress={removeProfilePhoto}
                                color="danger"
                                isDisabled={!fileImage}
                                title="Remove image"
                            >
                                Remove <MdDelete />
                            </Button>

                        </ButtonGroup>

                    </div>
                </CardBody>
            </Card>
        </>
    )
}

function UserName({ name }: { name: string }) {

    const [userName, setUserName] = useState<string>("")
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    useEffect(() => { setUserName(name) }, [name])

    async function updateName(): Promise<void> {

        if (!name) {
            return setAlert({
                isVisible: true,
                text: "Enter a name.",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {
            await patchAccount({ name: userName })
            setAlert({ text: "Name updated successfully.", status: "success", isVisible: true })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <>
            <Card className="m-3">
                <CardBody>

                    <label className="text-default-500 text-lg"> Name </label>

                    <Input
                        value={userName}
                        placeholder="Fulano"
                        variant="underlined"
                        className="mb-2"
                        onValueChange={value => setUserName(value)}
                    />

                    <Button
                        className="w-32"
                        onPress={updateName}
                        color="primary"
                    >
                        {isLoad ? <Spinner color="default" size="sm" /> : <> Save <MdSave /> </>}
                    </Button>

                </CardBody>
            </Card>
        </>
    )
}

function UserEmail({ email }: { email: string }) {

    const [userEmail, setUserEmail] = useState<string>("")
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    useEffect(() => { setUserEmail(email) }, [email])

    async function updateEmail(): Promise<void> {

        if (!email) {
            return setAlert({
                isVisible: true,
                text: "Enter a email.",
                status: "warning"
            });
        }

        setIsLoad(true)

        try {
            await patchAccount({ email: userEmail })
            setAlert({ text: "Email updated successfully.", status: "success", isVisible: true })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (

        <Card className="m-3">
            <CardBody>

                <label className="text-default-500 text-lg"> Name </label>

                <Input
                    type="email"
                    value={userEmail}
                    placeholder="exemple@email.com"
                    variant="underlined"
                    className="mb-2"
                    onValueChange={value => setUserEmail(value)}
                />

                <Button
                    className="w-32"
                    onPress={updateEmail}
                    color="primary"
                >
                    {isLoad ? <Spinner color="default" size="sm" /> : <> Save <MdSave /> </>}
                </Button>

            </CardBody>
        </Card>

    )
}

function UserPassword() {

    const [password, setPassword] = useState<string>("")
    const [confirmPassword, setConfirmPassword] = useState<string>("")
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)

    async function updatePassword() {

        if (!password) {
            return setAlert({
                isVisible: true,
                text: "Enter a new password.",
                status: "warning"
            });
        }

        if (!confirmPassword) {
            return setAlert({
                isVisible: true,
                text: "Confirm the new password.",
                status: "warning"
            });
        }

        if (password != confirmPassword) {
            return setAlert({
                text: "Passwords do not match.",
                status: "warning",
                isVisible: true
            });
        }

        setIsLoad(true)

        try {
            await patchAccount({ password: password })
            setAlert({ text: "Password updated successfully.", status: "success", isVisible: true })
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)

    }

    return (
        <>
            <Card className="m-3">
                <CardBody>
                    <label className="text-default-500 text-lg"> Security </label>

                    <InputPassword
                        label="New password"
                        setValue={value => setPassword(value)}
                    />

                    <InputPassword
                        label="Confirm password"
                        setValue={value => setConfirmPassword(value)}
                    />

                    <Button
                        color="primary"
                        className="mt-2 w-32"
                        onPress={updatePassword}
                        title="Fill in the fields to be able to save the changes"
                    >
                        {isLoad ? <Spinner color="default" size="sm" /> : <> Save <MdSave /> </>}
                    </Button>

                </CardBody>
            </Card>
        </>
    )
}

function UserRomoverAccount() {

    const [isOpenModal, setIsOpenModal] = useState<boolean>(false)
    const onChangeModal = (): void => setIsOpenModal(!isOpenModal)
    const [isLoad, setIsLoad] = useState<boolean>(false)
    const { setAlert } = useContext(MainContext)
    const navigate = useNavigate()

    async function removeAccount() {
        setIsLoad(true)

        try {
            await deleteAccount()
            localStorage.removeItem("token")
            navigate("/")
        } catch (error) {
            setAlert({ text: axiosErrorToString(error), status: "error", isVisible: true })
        }

        setIsLoad(false)
    }

    return (
        <>
            <Card className="m-3">

                <CardBody>

                    <label className="text-default-500 text-lg"> Remove account </label>

                    <p>When removing your account all your data will be permanently removed. </p>

                    <Button color="danger" onPress={onChangeModal} className="w-32 mt-2" > Remove <MdDelete /> </Button>

                    <Modal isOpen={isOpenModal}>

                        <ModalContent className="h-64">

                            <ModalHeader className="flex flex-col gap-1">Remove account</ModalHeader>

                            <ModalBody >

                                <p>
                                    When removing your account all your data will be permanently removed.
                                    Do you wish to continue?
                                </p>

                            </ModalBody>

                            <ModalFooter>

                                <Button color="primary" variant="light" onPress={() => setIsOpenModal(false)}>
                                    Cancel
                                </Button>

                                <Button onPress={removeAccount} color="danger">
                                    {isLoad ? <Spinner color="default" size="sm" /> : <>Confirm</>}
                                </Button>

                            </ModalFooter>

                        </ModalContent>

                    </Modal>

                </CardBody>

            </Card>
        </>
    )
}