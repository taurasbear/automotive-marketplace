import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { useState } from "react";
import { useCreateCar } from "../api/useCreateCar";
import { CarFormData } from "../types/CarFormData";
import CarForm from "./CarForm";

const CreateCarDialog = () => {
  const [isCreateCarDialogOpen, setIsCreateCarDialogOpen] = useState<boolean>();

  const { mutateAsync: createCarAsync } = useCreateCar();

  const handleSubmit = async (formData: CarFormData) => {
    await createCarAsync(formData);

    setIsCreateCarDialogOpen(false);
  };

  return (
    <Dialog
      open={isCreateCarDialogOpen}
      onOpenChange={setIsCreateCarDialogOpen}
    >
      <DialogTrigger asChild>
        <Button>Add car</Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Create new car</DialogTitle>
        </DialogHeader>
        <CarForm
          car={{
            year: 0,
            transmission: "",
            fuel: "",
            bodyType: "",
            drivetrain: "",
            doorCount: 0,
            makeId: "",
            modelId: "",
          }}
          onSubmit={handleSubmit}
        />
      </DialogContent>
    </Dialog>
  );
};

export default CreateCarDialog;
